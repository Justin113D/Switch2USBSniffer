extends PanelContainer

const Dataset = TauPlot.Dataset
const AxisId = TauPlot.AxisId
const PaneOverlayType = TauPlot.PaneOverlayType
const MarkerShape = TauScatterStyle.MarkerShape
const LegendPosition = TauLegendConfig.Position
const ColorBuffer = TauPlot.ColorBuffer
const Float32Buffer = TauPlot.Float32Buffer
const ScatterVisualCallbacks = TauPlot.ScatterVisualCallbacks
const LineVisualCallbacks = TauPlot.LineVisualCallbacks

####################################################################################################

const SAMPLE_RATE := 250

# 5 seconds worth of samples
const SAMPLE_COUNT = SAMPLE_RATE * 5;

const SERIES_GROUP_SIZES: Array[int] = [4, 3, 3]
const SERIES_GROUP_LABELS : Array[String] = [
	"Quaternion",
	"Accel",
	"Gyro"
]
const SERIES_LABELS : Array[String] = [
	"Quaternion W", 
	"Quaternion X",
	"Quaternion Y",
	"Quaternion Z",
	
	"Accel X",
	"Accel Y",
	"Accel Z",
	
	"Gyro X",
	"Gyro Y",
	"Gyro Z",
]

var _dataset: Dataset
var _series_ids: Array[int]
var _cursor: int

func create_pane(
	title: String,
	min_override: float,
	max_override: float,
	stretch: float
) -> TauPaneConfig:
	
	var axis := TauAxisConfig.new()
	axis.type = TauAxisConfig.Type.CONTINUOUS
	axis.title = title
	axis.range_override_enabled = true
	axis.min_override = min_override
	axis.max_override = max_override
	axis.tick_count_preferred = 5
	
	var line := TauLineConfig.new()
	line.gap_policy = TauLineConfig.GapPolicy.SKIP
	
	var grid := TauGridLineConfig.new()
	grid.x_major_enabled = true
	grid.y_major_enabled = true
	
	var pane := TauPaneConfig.new()
	pane.y_left_axis = axis
	pane.overlays = [line]
	pane.grid_line = grid
	pane.stretch_ratio = stretch
	
	return pane

func _ready() -> void:
	_cursor = 0
	var plot = $GyroPlot
	
	plot.title = "[b]Gyro History[/b]"
	plot.legend_enabled = true
	plot.hover_enabled = false

	var x := PackedFloat64Array()
	x.resize(SAMPLE_COUNT)
	for i in range(SAMPLE_COUNT):
		x[i] = float(i) / SAMPLE_RATE
		
	var blank := PackedFloat64Array()
	blank.resize(SAMPLE_COUNT)
	blank.fill(NAN)
	
	_dataset = Dataset.make_shared_x_continuous(
		PackedStringArray(SERIES_LABELS), x,
		[
			blank, blank, blank, blank, 
			blank, blank, blank,
			blank, blank, blank
		]
	)
	
	_series_ids = []
	for i in len(SERIES_LABELS):
		_series_ids.append(_dataset.get_series_id_by_index(i))
	
	var x_axis := TauAxisConfig.new()
	x_axis.type = TauAxisConfig.Type.CONTINUOUS
	x_axis.range_override_enabled = true
	x_axis.min_override = 0.0
	x_axis.max_override = 5.0
	x_axis.tick_count_preferred = 22
	
	var quaternion_pane = create_pane(SERIES_GROUP_LABELS[0], -1.0, 1.0, 2.0)
	var accel_pane = create_pane(SERIES_GROUP_LABELS[1], -3, 3, 1.0)
	var gyro_pane = create_pane(SERIES_GROUP_LABELS[2], -32767.0, 32767.0, 1.0)
	
	var xy := TauXYConfig.new()
	xy.x_axis_id = AxisId.BOTTOM
	xy.x_axis = x_axis
	xy.panes = [quaternion_pane, accel_pane, gyro_pane]
	
	var bindings: Array[TauXYSeriesBinding] = []
	var offset: int = 0
	var group_index: int = 0
	
	for group_size in SERIES_GROUP_SIZES:
		
		for i in group_size:
			var binding := TauXYSeriesBinding.new()
			binding.series_id = _series_ids[i + offset]
			binding.pane_index = group_index 
			binding.overlay_type = PaneOverlayType.LINE
			binding.y_axis_id = AxisId.LEFT
			bindings.append(binding)
			
		offset += group_size
		group_index += 1

	plot.plot_xy(_dataset, xy, bindings)

func update_plots(values: Array[PackedFloat64Array]):
		
	var head := SAMPLE_COUNT - _cursor
	var longest = 0
	
	_dataset.begin_batch()
	for i in values.size():
		var arr = values[i];
		var series_id = _series_ids[i]
		
		if arr.size() <= head:
			_dataset.set_series_y_slice(series_id, _cursor, arr.slice(0, head))
			_dataset.set_series_y_slice(series_id, 0, arr.slice(head))
		else:
			_dataset.set_series_y_slice(series_id, _cursor, arr)
			
		if arr.size() > longest:
			longest = arr.size()
			
	_dataset.end_batch()
	
	_cursor = (_cursor + longest - 1) % SAMPLE_COUNT
	

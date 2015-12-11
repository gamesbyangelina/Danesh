using UnityEngine;
using System.Collections;

public class TargetSetting {

	public delegate float MetricDelegate(Tile[,] map);

	public MetricDelegate evaluateForMetric;

	public float targetValue;

	public TargetSetting(MetricDelegate m, float target){
		this.evaluateForMetric = m;
		this.targetValue = target;
	}
}

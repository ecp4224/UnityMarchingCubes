using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobMonitor : BindableMonoBehavior
{

	[BindComponent]
	public Text text;

	[BindComponent(fromObject = "Voxel World")]
	private SampledChunkGenerator temp;
	
	// Update is called once per frame
	void Update ()
	{
		text.text = "Chunks In Queue: " + temp.JobsInQueue + 
		            "\nChunks Generated: " + temp.JobsCompleted +
					"\nAverage Generation Time: " + temp.TimeSpentAverage + " seconds";
	}
}

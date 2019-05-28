using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public abstract class ChunkGenerator<T> : ChunkProvider where T : struct, IJob 
{
    [BindComponent] [HideInInspector] 
    protected World world;

    [BindComponent(skipIfNonNull = true, warnOnly = true)]
    public ChunkFileLoader fileLoader;
    
    struct JobHolder
    {
        internal T job;
        internal JobHandle handle;
        internal float startTime;
    }
    
    private Dictionary<Vector3, JobHolder> _jobs = new Dictionary<Vector3, JobHolder>();

    public int JobsInQueue
    {
        get { return _jobs.Count; }
    }
    
    public int JobsCompleted { get; private set; }
    public float TimeSpentAverage { get; private set; }
    private float runningSum = 0f;
    
    public override Chunk LoadChunkAt(Vector3 worldOrigin)
    {
        int size = world.chunkSize;
        float scale = world.voxelSize;
        
        int buffer = size + 1;

        if (!_jobs.ContainsKey(worldOrigin))
        {
            var job = CreateJob(worldOrigin);

            var handle = job.Schedule();
            
            //Check to see if we completed it
            if (handle.IsCompleted)
            {
                handle.Complete();

                var sample = ChunkFromJob(job);
                
                var c = new Chunk(world.OriginToPoint(worldOrigin), sample, size, world.Height);
            
                c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(worldOrigin);

                return c;
            }
            
            var holder = new JobHolder()
            {
                job = job,
                handle = handle,
                startTime = Time.time
            };
            
            _jobs.Add(worldOrigin, holder);
        }
        else
        {
            var holder = _jobs[worldOrigin];

            if (holder.handle.IsCompleted)
            {
                holder.handle.Complete();

                var sample = ChunkFromJob(holder.job);
                
                var c = new Chunk(world.OriginToPoint(worldOrigin), sample, size, world.Height);
            
                c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(worldOrigin);

                float duration = Time.time - holder.startTime;

                runningSum += duration;

                JobsCompleted++;

                TimeSpentAverage = runningSum / JobsCompleted;

                return c;
            }
        }

        return null;
    }

    protected abstract T CreateJob(Vector3 worldOrigin);

    protected abstract float[] ChunkFromJob(T job);

    private void OnDestroy()
    {
        foreach (var key in _jobs.Keys)
        {
            var holder = _jobs[key];
            
            holder.handle.Complete(); //Complete the job before we die

            ChunkFromJob(holder.job); //Ensure we clear the chunk data
        }
    }
}
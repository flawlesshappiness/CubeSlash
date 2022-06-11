using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITargetController : Singleton
{
    public static AITargetController Instance { get { return Instance<AITargetController>(); } }
    private Dictionary<Transform, Artifact> artifacts = new Dictionary<Transform, Artifact>();

    public bool RequestArtifact(Enemy enemy, Transform target)
    {
        var artifact = GetOrCreateArtifact(target);
        if (artifact.AtMax)
        {
            return false;
        }
        else
        {
            artifact.owners.Add(enemy);
            return true;
        }
    }

    public void RemoveArtifact(Enemy enemy, Transform target)
    {
        var artifact = GetOrCreateArtifact(target);
        if (artifact.owners.Contains(enemy))
        {
            artifact.owners.Remove(enemy);
        }
    }

    public void ClearArtifacts(Enemy enemy)
    {
        artifacts.Values
            .Where(a => a.owners.Contains(enemy))
            .ToList().ForEach(a => a.owners.Remove(enemy));
    }

    public void ClearArtifacts()
    {
        artifacts.Clear();
    }

    public void SetArtifactOwnerCount(Transform target, int count)
    {
        var artifact = GetOrCreateArtifact(target);
        artifact.max = count;
    }

    private Artifact GetOrCreateArtifact(Transform target)
    {
        if (!artifacts.ContainsKey(target))
        {
            artifacts.Add(target, new Artifact { target = target, max = -1 });
        }
        return artifacts[target];
    }

    private class Artifact
    {
        public int max;
        public List<Enemy> owners = new List<Enemy>();
        public Transform target;
        
        public bool AtMax { get { return max >= 0 && owners.Count >= max; } }
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace project1.scripts.world.entity.ai.schedule;

public class Timeline
{
    private readonly List<Keyframe> _keyframes = new ();
    private int _previousIndex = 0;
    
    public ReadOnlyCollection<Keyframe> Keyframes => _keyframes.AsReadOnly();
    
    public Timeline AddKeyframe(int timeStamp, float value)
    {
        _keyframes.Add(new Keyframe(timeStamp, value));
        SortAndDeduplicateKeyframes();
        return this;
    }
    
    public Timeline AddKeyframes(IEnumerable<Keyframe> keyframes)
    {
        _keyframes.AddRange(keyframes);
        SortAndDeduplicateKeyframes();
        return this;
    }
    
    private void SortAndDeduplicateKeyframes()
    {
        _keyframes.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));
        _previousIndex = 0;
    }
    
    public float GetValueAt(int timeStamp)
    {
        if (_keyframes.Count <= 0)
        {
            return 0.0f;
        }
        
        Keyframe keyframe = _keyframes[_previousIndex];
        Keyframe keyframe2 = _keyframes[^1];
        
        bool isBeforeFirst = timeStamp < keyframe.TimeStamp;
        int index = isBeforeFirst ? 0 : _previousIndex;
        float value = isBeforeFirst ? keyframe2.Value : keyframe.Value;
        
        int k = index;
        while (k < _keyframes.Count && _keyframes[k].TimeStamp <= timeStamp)
        {
            _previousIndex = k++;
            value = _keyframes[k].Value;
        }
        
        return value;
    }
}
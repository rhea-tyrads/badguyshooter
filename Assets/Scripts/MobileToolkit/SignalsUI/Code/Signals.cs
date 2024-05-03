using System;
using System.Collections.Generic;
using Utilities.MonoCache.System;

public class Signals : Singleton<Signals>
{
    public SignalUI[] signals;
    public List<SignalState> states = new();
    public event Action<SignalState> OnRefresh = delegate { };
    public SignalListener[] listeners;

    void Start()
    {
        InitStates();
        FindSignals();
        InitSignals();


        Invoke(nameof(Init), 0.3f);

    }
    void Init()
    {
        FindListeners();
        InitListeners();
    }

    void Refresh()
    {
        foreach (var signal in signals)
            signal.Refresh(states);
    }

    void FindSignals()
    {
        signals = FindObjectsOfType<SignalUI>();
    }

    void FindListeners()
    {
        listeners = GetComponentsInChildren<SignalListener>();
    }

    void InitListeners()
    {
        foreach (var listener in listeners)
        {
            listener.OnChange += Refresh;
            listener.Init();
        }
    }

    void Refresh(SignalState state)
    {
        foreach (var signal in signals)
            signal.Refresh(state);
    }

    void InitSignals()
    {
        foreach (SignalUI signal in signals)
        {
            signal.Init();
            signal.Hide();
        }
    }

    void InitStates()
    {
        foreach (SignalType type in Enum.GetValues(typeof(SignalType)))
        {
            if (type == SignalType.None) continue;

            var debug = new SignalState();
            debug.type = type;
            states.Add(debug);
        }
    }
}

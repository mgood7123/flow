using Debug = ScriptManager.Debug;
using UnityEngine;
using System.Collections.Generic;

public class Main {

    ScriptManager.ThreadRunner tr;

    Canvas canvas;

    public void onStart(ScriptManager.ThreadRunner tr, GameObject gameObject) {
        this.tr = tr;
        canvas = gameObject.AddComponent<Canvas>();

        tr.RunOnBackground(() => {
            reload_finish(null);
        });
    }

    public void reload_begin(List<object> data) {
        Debug.OnMessageEmitted -= onEmit;
    }

    public void reload_finish(List<object> data) {
        Debug.OnMessageEmitted += onEmit;
    }

    void onEmit(string text) {
    }
}

using Debug = ScriptManager.Debug;
using UnityEngine;
using static ScriptManager;
using static ScriptManager.RunnableUtils;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class Script : MonoBehaviour {

    ScriptManager sm;
    ThreadRunner tr;

    GameObject obj;
    private UIDocument ui;

    // Start is called before the first frame update
    void Start() {
        Debug.Log("START");

        tr = new ThreadRunner();
        obj = gameObject;

        sm = new ScriptManager(onScriptLoaded,
                            "./scripts"
                            , typeof(UnityEngine.Object)
                            , typeof(UnityEngine.UIElements.UIDocument)
                            , typeof(Debug)
                            , typeof(UnityEngine.Canvas)
        //,typeof(Unity.Burst.BurstCompilerOptions),
        //,typeof(Unity.Burst.BurstRuntime),
        //,typeof(Unity.Burst.BurstCompiler),
        //,typeof(Unity.Burst.BurstDiscardAttribute),
        //,typeof(Unity.Burst.BurstCompileAttribute),
        //,typeof(Unity.Burst.BurstAuthorizedExternalMethodAttribute),
        //,typeof(Unity.Burst.NoAliasAttribute),
        //,typeof(Unity.Collections.BurstCompatibleAttribute),
        //,typeof(Unity.Collections.NotBurstCompatibleAttribute)
        );
    }

    private void onScriptLoaded(CompileInfo compileInfo) {
        Debug.Log("onScriptLoaded : " + compileInfo.Path);
        Debug.Log("precompiling Main.onStart : " + compileInfo.Path);
        if (sm.precompile<Runnable<ThreadRunner, GameObject>>(compileInfo, "Main", "onStart", out var del)) {
            Debug.Log("precompiled Main.onStart : " + compileInfo.Path);
            tr.RunOnMain(_ => del(compileInfo, "Main", "onStart", tr, obj));
        }
    }

    void Update() {
        tr.RunMainLoop();
        sm.invoke_callback<Runnable>("Main", "update");
    }
}
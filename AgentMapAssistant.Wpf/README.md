# AgentMapAssistant — WPF

Ask for map operations in plain language ("Show the buildings within 150 m of a school in
red") and watch them run as a validated, previewed and recorded GeoKernel workflow. The
planner (an LLM, or the offline demo) only proposes a **workflow DSL v2** plan (ADR-0007);
GeoKernel validates it, the plan is shown before anything runs, steps that change data need
your confirmation of that exact plan, and every executed workflow can be undone and is
recorded with hash-chained provenance. WPF port of the Qt `AgentMapAssistant` example;
the conversation (planner, evaluate, run, undo, provenance) is shared with the
[Winforms version](../AgentMapAssistant.Winforms/README.md) in
[`AgentConversation.cs`](../Common/AgentMap/AgentConversation.cs), on top of
GeoKernel.NET's `GisAgent` and `GisLlm.Propose`.

## Requirements and build

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The agent API ships after
GeoKernel.NET **1.5.40**; until the next release, build against the GeoKernel.NET sources
and put a native GeoKernel runtime with the agent API on `PATH`:

```powershell
$env:Path = "D:\projects\GeoKernel\outputs\build\Release;" + $env:Path
dotnet run -c Release --project AgentMapAssistant.Wpf -p:GeoKernelNetSource=D:/projects/GeoKernel.NET
```

After the release, use the NuGet package instead: `-p:GeoKernelSdkVersion=<version>`.
Without either property the build stops with an explanation.

## Planner: offline demo or an LLM

Without configuration the example runs in **offline demo mode**: the six requests in the
**Try** list have prepared plans in
[`agent-demo.json`](../Common/AgentMap/demo-data/agent-demo.json), exactly as a model would
return them, and they go through the same validation, preview, confirmation, execution,
undo and provenance. Other requests are answered with how to enable a model. No network
access is needed.

To plan your own requests, set the provider before starting:

| Provider | Environment |
|---|---|
| Anthropic | `GEOKERNEL_LLM_PROVIDER=anthropic`, `ANTHROPIC_API_KEY` (model defaults to `claude-opus-5-5`) |
| OpenAI | `GEOKERNEL_LLM_PROVIDER=openai`, `GEOKERNEL_LLM_MODEL`, `OPENAI_API_KEY` |
| Ollama (local) | `GEOKERNEL_LLM_PROVIDER=ollama`, `GEOKERNEL_LLM_MODEL`, optional `GEOKERNEL_LLM_BASE_URL` |

The model call runs on a worker thread. Keys are read by the native code when a request is
made and never appear in the log, errors or provenance. The model receives the request,
the layer list with field names and the field notes from `agent-demo.json` — no feature
data.

## Workflow

1. The map shows a synthetic neighbourhood: 352 buildings (`ref`, `use`, `floors`,
   `year_built`, `status`), 4 schools, 3 parks and 16 streets.
2. Pick a request under **Try** and click **Ask**, or type one and click **Send**.
3. The **Plan** box lists every step with its category, effects and parameters before
   anything runs. A plan that fails validation is rejected with the reason.
4. **Run** executes read-only and view-only plans. Plans that change or delete data show
   **Confirm and run**, which passes that plan's `planId` as the confirmation.
5. The log shows results: tables row by row, measurements with units, feature counts.
6. **Undo last** reverts every step of the most recent workflow.
7. **Provenance...** shows the hash-chained execution records.

Edits only apply to result layers in memory; the GeoJSON files are never modified. The data
is a synthetic, deterministic neighbourhood shared with the Qt, Python, Electron and Rust
versions (street names borrowed from Kadıköy, Istanbul; geometry and attributes invented).

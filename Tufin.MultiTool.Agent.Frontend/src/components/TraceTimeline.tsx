import { CheckCircle2, Clock, Database, MessageCircle, Play, Wrench, XCircle } from 'lucide-react';
import type { AgentTraceEvent } from '../types';

const iconByType: Record<string, typeof Play> = {
  TaskStarted: Play,
  ModelDecision: MessageCircle,
  ToolCall: Wrench,
  ToolResult: Database,
  FinalAnswer: CheckCircle2,
  TaskCompleted: CheckCircle2,
  TaskFailed: XCircle
};

export default function TraceTimeline({ trace }: { trace: AgentTraceEvent[] }) {
  if (!trace.length) {
    return <div className="card text-slate-400">No trace events.</div>;
  }

  return (
    <div className="card">
      <div className="mb-5 flex items-center justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Observable trace</p>
          <h2 className="text-2xl font-bold">Reasoning timeline</h2>
        </div>
        <span className="pill">{trace.length} events</span>
      </div>

      <div className="space-y-4">
        {trace.map((event) => {
          const Icon = iconByType[event.eventType] ?? Clock;
          return (
            <details key={event.id} className="rounded-2xl border border-white/10 bg-slate-950/60 p-4" open={event.eventType === 'ToolCall' || event.eventType === 'ToolResult'}>
              <summary className="cursor-pointer list-none">
                <div className="flex flex-wrap items-center gap-3">
                  <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-cyan-400/10 text-cyan-200">
                    <Icon size={18} />
                  </span>
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-semibold">#{event.sequence} {event.eventType}</span>
                      {event.toolName && <span className="pill">{event.toolName}</span>}
                      {event.latencyMs !== undefined && <span className="pill">{event.latencyMs} ms</span>}
                    </div>
                    <p className="mt-1 text-sm text-slate-400">{event.decisionSummary}</p>
                  </div>
                </div>
              </summary>

              <div className="mt-4 grid gap-3 md:grid-cols-2">
                <Info label="Step" value={String(event.stepNumber)} />
                <Info label="Occurred" value={new Date(event.occurredAt).toLocaleString()} />
                {event.tokenUsage && <Info label="Tokens" value={String(event.tokenUsage.totalTokens)} />}
                {event.error && <Info label="Error" value={event.error} danger />}
              </div>

              {(event.arguments !== undefined || event.result !== undefined) && (
                <div className="mt-4 grid gap-4 lg:grid-cols-2">
                  {event.arguments !== undefined && (
                    <pre className="json-block">{JSON.stringify(event.arguments, null, 2)}</pre>
                  )}
                  {event.result !== undefined && (
                    <pre className="json-block">{JSON.stringify(event.result, null, 2)}</pre>
                  )}
                </div>
              )}
            </details>
          );
        })}
      </div>
    </div>
  );
}

function Info({ label, value, danger }: { label: string; value: string; danger?: boolean }) {
  return (
    <div className="rounded-xl border border-white/10 bg-white/[0.03] px-3 py-2">
      <p className="text-xs uppercase tracking-[0.18em] text-slate-500">{label}</p>
      <p className={`mt-1 text-sm ${danger ? 'text-rose-300' : 'text-slate-200'}`}>{value}</p>
    </div>
  );
}

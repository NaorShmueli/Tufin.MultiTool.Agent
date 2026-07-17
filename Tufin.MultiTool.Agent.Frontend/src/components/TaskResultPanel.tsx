import { Braces, Clock3, Cpu, Hash } from 'lucide-react';
import { useState } from 'react';
import type { AgentTaskResponse } from '../types';
import JsonDialog from './JsonDialog';
import TraceTimeline from './TraceTimeline';

export default function TaskResultPanel({ response }: { response: AgentTaskResponse }) {
  const [jsonOpen, setJsonOpen] = useState(false);

  return (
    <div className="space-y-6">
      <div className="card">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Final answer</p>
            <h2 className="mt-2 whitespace-pre-wrap text-2xl font-bold leading-relaxed">{response.finalAnswer ?? response.error ?? 'No final answer'}</h2>
          </div>
          <button className="secondary-button flex items-center gap-2" onClick={() => setJsonOpen(true)}>
            <Braces size={16} />
            View Full JSON
          </button>
        </div>

        <div className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Metric icon={Hash} label="Task ID" value={response.taskId.slice(0, 8)} />
          <Metric icon={Cpu} label="Model" value={response.model} />
          <Metric icon={Clock3} label="Latency" value={`${response.metrics.totalLatencyMs ?? 0} ms`} />
          <Metric icon={Braces} label="Tokens" value={String(response.metrics.totalTokens)} />
        </div>
      </div>

      <TraceTimeline trace={response.trace} />

      <JsonDialog title={`Task ${response.taskId}`} data={response} open={jsonOpen} onClose={() => setJsonOpen(false)} />
    </div>
  );
}

function Metric({ icon: Icon, label, value }: { icon: typeof Hash; label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-slate-950/60 p-4">
      <div className="flex items-center gap-3 text-slate-400">
        <Icon size={17} />
        <span className="text-xs uppercase tracking-[0.18em]">{label}</span>
      </div>
      <p className="mt-2 truncate text-lg font-semibold text-slate-100">{value}</p>
    </div>
  );
}

import { Loader2, Send } from 'lucide-react';
import { useState } from 'react';
import { submitTask } from '../api';
import TaskResultPanel from '../components/TaskResultPanel';
import type { AgentTaskResponse } from '../types';

const examples = [
  'What is the cost of iPhone 17?',
  'Give me all products name and prices',
  'What is the total value of order 1001?',
  'Convert 10 kilometers to miles',
  'What is the current weather in London?'
];

export default function ChatPage() {
  const [task, setTask] = useState(examples[0]);
  const [response, setResponse] = useState<AgentTaskResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function onSubmit() {
    if (!task.trim()) return;

    setLoading(true);
    setError(null);
    setResponse(null);

    try {
      setResponse(await submitTask(task.trim()));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Task submission failed.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="space-y-6">
      <section className="card">
        <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Single task chat</p>
        <h1 className="mt-2 text-3xl font-bold">Ask the agent</h1>
        <p className="mt-3 max-w-3xl text-slate-400">
          Each submission creates a new independent task. This UI intentionally does not implement multi-turn context.
        </p>

        <div className="mt-6 grid gap-4 lg:grid-cols-[1fr_auto]">
          <textarea
            className="input min-h-32 resize-y"
            value={task}
            onChange={(event) => setTask(event.target.value)}
            placeholder="Describe one task for the agent..."
          />
          <button className="primary-button flex h-12 items-center justify-center gap-2 self-end" disabled={loading || !task.trim()} onClick={onSubmit}>
            {loading ? <Loader2 className="animate-spin" size={18} /> : <Send size={18} />}
            Run task
          </button>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          {examples.map((example) => (
            <button key={example} className="secondary-button text-sm" onClick={() => setTask(example)}>
              {example}
            </button>
          ))}
        </div>
      </section>

      {error && <div className="rounded-2xl border border-rose-400/30 bg-rose-500/10 p-4 text-rose-200">{error}</div>}
      {response && <TaskResultPanel response={response} />}
    </div>
  );
}

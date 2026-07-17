import { Loader2, Search } from 'lucide-react';
import { useEffect, useState } from 'react';
import { getTaskById, getTasks } from '../api';
import TaskResultPanel from '../components/TaskResultPanel';
import type { AgentTaskListItem, AgentTaskResponse } from '../types';

export default function TasksPage() {
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [take, setTake] = useState(25);
  const [tasks, setTasks] = useState<AgentTaskListItem[]>([]);
  const [selected, setSelected] = useState<AgentTaskResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedLoading, setSelectedLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadTasks() {
    setLoading(true);
    setError(null);
    try {
      setTasks(await getTasks({ from: toIso(from), to: toIso(to), take }));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not load tasks.');
    } finally {
      setLoading(false);
    }
  }

  async function selectTask(taskId: string) {
    setSelectedLoading(true);
    setError(null);
    try {
      setSelected(await getTaskById(taskId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not load task.');
    } finally {
      setSelectedLoading(false);
    }
  }

  useEffect(() => {
    void loadTasks();
  }, []);

  return (
    <div className="space-y-6">
      <section className="card">
        <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Persistent task log</p>
        <h1 className="mt-2 text-3xl font-bold">Past tasks</h1>
        <p className="mt-3 text-slate-400">Query persisted task records and click any row to retrieve the full response and trace.</p>

        <div className="mt-6 grid gap-3 md:grid-cols-[1fr_1fr_120px_auto]">
          <label className="grid gap-1 text-sm text-slate-400">
            From
            <input className="input" type="date" value={from} onChange={(event) => setFrom(event.target.value)} />
          </label>
          <label className="grid gap-1 text-sm text-slate-400">
            To
            <input className="input" type="date" value={to} onChange={(event) => setTo(event.target.value)} />
          </label>
          <label className="grid gap-1 text-sm text-slate-400">
            Take
            <input className="input" type="number" min={1} max={200} value={take} onChange={(event) => setTake(Number(event.target.value))} />
          </label>
          <button className="primary-button mt-6 flex items-center justify-center gap-2" disabled={loading} onClick={loadTasks}>
            {loading ? <Loader2 className="animate-spin" size={18} /> : <Search size={18} />}
            Search
          </button>
        </div>
      </section>

      {error && <div className="rounded-2xl border border-rose-400/30 bg-rose-500/10 p-4 text-rose-200">{error}</div>}

      <div className="grid gap-6 xl:grid-cols-[420px_1fr]">
        <section className="card h-fit">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-xl font-bold">Results</h2>
            <span className="pill">{tasks.length} tasks</span>
          </div>
          <div className="space-y-3">
            {tasks.map((task) => (
              <button
                key={task.taskId}
                className="w-full rounded-2xl border border-white/10 bg-slate-950/60 p-4 text-left transition hover:border-cyan-300/40 hover:bg-white/10"
                onClick={() => selectTask(task.taskId)}
              >
                <div className="mb-2 flex items-center justify-between gap-3">
                  <span className="pill">{task.status}</span>
                  <span className="text-xs text-slate-500">{new Date(task.createdAt).toLocaleString()}</span>
                </div>
                <p className="line-clamp-2 font-medium text-slate-100">{task.input}</p>
                <p className="mt-2 text-xs text-slate-500">{task.taskId}</p>
              </button>
            ))}
          </div>
        </section>

        <section>
          {selectedLoading && <div className="card flex items-center gap-3 text-slate-300"><Loader2 className="animate-spin" /> Loading task...</div>}
          {!selectedLoading && selected && <TaskResultPanel response={selected} />}
          {!selectedLoading && !selected && <div className="card text-slate-400">Select a task to inspect its full response.</div>}
        </section>
      </div>
    </div>
  );
}

function toIso(value: string): string | undefined {
  return value ? new Date(value).toISOString() : undefined;
}

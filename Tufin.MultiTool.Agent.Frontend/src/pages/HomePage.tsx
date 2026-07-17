import { ArrowRight, BrainCircuit, Calculator, CloudSun, Database, Eye, Gauge, Ruler } from 'lucide-react';
import { Link } from 'react-router-dom';

const capabilities = [
  { icon: Calculator, title: 'Calculator', text: 'Safely evaluates arithmetic expressions with deterministic backend code.' },
  { icon: CloudSun, title: 'Weather', text: 'Fetches live current weather through Open-Meteo.' },
  { icon: Ruler, title: 'Unit Converter', text: 'Converts length, weight, and temperature units.' },
  { icon: Database, title: 'Database Query', text: 'Queries a seeded SQLite product catalog with read-only SQL.' },
  { icon: Eye, title: 'Observability', text: 'Persists every model decision, tool call, result, latency, and token usage.' },
  { icon: BrainCircuit, title: 'Local Model', text: 'Runs with Ollama and does not require external LLM API keys.' }
];

export default function HomePage() {
  return (
    <div className="space-y-10">
      <section className="glass overflow-hidden rounded-3xl p-8 lg:p-12">
        <div className="grid gap-10 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
          <div>
            <span className="pill mb-5">Multi-tool AI agent with structured traces</span>
            <h1 className="max-w-4xl text-4xl font-black tracking-tight sm:text-6xl">
              Solve tasks with tools, then inspect every step.
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-300">
              This service accepts a natural-language task, lets a local Ollama model decide which backend tools to call, records each observable action, and returns both the final answer and the complete trace.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link to="/chat" className="primary-button inline-flex items-center gap-2">
                Try the agent <ArrowRight size={18} />
              </Link>
              <Link to="/tasks" className="secondary-button inline-flex items-center gap-2">
                View persisted tasks
              </Link>
            </div>
          </div>

          <div className="rounded-3xl border border-cyan-300/20 bg-slate-950/80 p-5 shadow-2xl shadow-cyan-500/10">
            <div className="mb-4 flex items-center gap-3">
              <Gauge className="text-cyan-300" />
              <div>
                <p className="text-sm font-semibold text-cyan-200">Reasoning loop</p>
                <p className="text-xs text-slate-500">observe {'->'} decide {'->'} act {'->'} observe</p>
              </div>
            </div>
            {['User task accepted', 'Model selects tool', 'Backend validates and executes', 'Tool result is logged', 'Model writes final answer', 'Trace is persisted'].map((step, index) => (
              <div key={step} className="mb-3 flex items-center gap-3 rounded-2xl border border-white/10 bg-white/[0.03] p-3">
                <span className="flex h-8 w-8 items-center justify-center rounded-xl bg-cyan-400 text-sm font-bold text-slate-950">{index + 1}</span>
                <span className="text-sm text-slate-200">{step}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section>
        <div className="mb-5">
          <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Capabilities</p>
          <h2 className="mt-2 text-3xl font-bold">What the agent can do</h2>
        </div>
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {capabilities.map((item) => (
            <div key={item.title} className="card">
              <item.icon className="mb-4 text-cyan-300" size={28} />
              <h3 className="text-xl font-bold">{item.title}</h3>
              <p className="mt-2 text-sm leading-6 text-slate-400">{item.text}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

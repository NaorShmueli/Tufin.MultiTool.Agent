import { Activity, Bot, Database, Home, MessageSquareText } from 'lucide-react';
import { NavLink, Route, Routes } from 'react-router-dom';
import ChatPage from './pages/ChatPage';
import HomePage from './pages/HomePage';
import TasksPage from './pages/TasksPage';

const navItems = [
  { to: '/', label: 'Overview', icon: Home },
  { to: '/chat', label: 'Chat', icon: MessageSquareText },
  { to: '/tasks', label: 'Tasks', icon: Database }
];

export default function App() {
  return (
    <div className="min-h-screen text-slate-100">
      <header className="sticky top-0 z-30 border-b border-white/10 bg-slate-950/80 backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <NavLink to="/" className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-2xl bg-cyan-400 text-slate-950 shadow-lg shadow-cyan-400/20">
              <Bot size={22} />
            </span>
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.28em] text-cyan-200">Tufin</p>
              <h1 className="text-lg font-bold">Multi-Tool Agent</h1>
            </div>
          </NavLink>

          <nav className="flex items-center gap-2 rounded-2xl border border-white/10 bg-white/5 p-1">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === '/'}
                className={({ isActive }) =>
                  `flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium transition ${
                    isActive ? 'bg-cyan-400 text-slate-950' : 'text-slate-300 hover:bg-white/10 hover:text-white'
                  }`
                }
              >
                <item.icon size={16} />
                <span className="hidden sm:inline">{item.label}</span>
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/chat" element={<ChatPage />} />
          <Route path="/tasks" element={<TasksPage />} />
        </Routes>
      </main>

      <footer className="mx-auto flex max-w-7xl items-center gap-2 px-4 pb-8 text-sm text-slate-500 sm:px-6 lg:px-8">
        <Activity size={16} />
        <span>Observable local-agent runtime with Ollama, SQLite, and structured traces.</span>
      </footer>
    </div>
  );
}

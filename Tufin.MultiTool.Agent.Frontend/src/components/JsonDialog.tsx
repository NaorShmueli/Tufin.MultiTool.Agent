import { Copy, X } from 'lucide-react';

type Props = {
  title: string;
  data: unknown;
  open: boolean;
  onClose: () => void;
};

export default function JsonDialog({ title, data, open, onClose }: Props) {
  if (!open) return null;

  const json = JSON.stringify(data, null, 2);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 p-4 backdrop-blur-sm">
      <div className="glass w-full max-w-5xl rounded-3xl">
        <div className="flex items-center justify-between border-b border-white/10 px-5 py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.25em] text-cyan-200">Raw response</p>
            <h2 className="text-xl font-bold">{title}</h2>
          </div>
          <div className="flex items-center gap-2">
            <button className="secondary-button" onClick={() => navigator.clipboard.writeText(json)}>
              <Copy size={16} />
            </button>
            <button className="secondary-button" onClick={onClose}>
              <X size={16} />
            </button>
          </div>
        </div>
        <div className="p-5">
          <pre className="json-block">{json}</pre>
        </div>
      </div>
    </div>
  );
}

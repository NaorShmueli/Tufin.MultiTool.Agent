export type AgentTaskStatus =
  | 'Pending'
  | 'Running'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'
  | 'MaxStepsExceeded';

export type TokenUsage = {
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
};

export type TaskMetrics = {
  totalLatencyMs?: number;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
};

export type AgentTraceEvent = {
  id: string;
  sequence: number;
  stepNumber: number;
  eventType: string;
  occurredAt: string;
  decisionSummary?: string;
  toolName?: string;
  arguments?: unknown;
  result?: unknown;
  latencyMs?: number;
  tokenUsage?: TokenUsage;
  error?: string;
};

export type AgentTaskResponse = {
  taskId: string;
  input: string;
  model: string;
  status: AgentTaskStatus;
  finalAnswer?: string;
  error?: string;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  metrics: TaskMetrics;
  trace: AgentTraceEvent[];
};

export type AgentTaskListItem = {
  taskId: string;
  input: string;
  status: AgentTaskStatus;
  createdAt: string;
};

export type HealthResponse = {
  status: string;
  totalDurationMs: number;
  checks: Array<{
    name: string;
    status: string;
    description?: string;
    durationMs: number;
    error?: string | null;
  }>;
};

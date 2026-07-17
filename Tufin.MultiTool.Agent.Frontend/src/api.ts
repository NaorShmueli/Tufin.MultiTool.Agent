import type { AgentTaskListItem, AgentTaskResponse, HealthResponse } from './types';

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers
    },
    ...init
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Request failed with HTTP ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export function submitTask(task: string): Promise<AgentTaskResponse> {
  return request<AgentTaskResponse>('/task', {
    method: 'POST',
    body: JSON.stringify({ task })
  });
}

export function getTasks(params: {
  from?: string;
  to?: string;
  take?: number;
}): Promise<AgentTaskListItem[]> {
  const query = new URLSearchParams();

  if (params.from) query.set('from', params.from);
  if (params.to) query.set('to', params.to);
  if (params.take) query.set('take', String(params.take));

  const suffix = query.toString() ? `?${query.toString()}` : '';
  return request<AgentTaskListItem[]>(`/tasks${suffix}`);
}

export function getTaskById(taskId: string): Promise<AgentTaskResponse> {
  return request<AgentTaskResponse>(`/tasks/${taskId}`);
}

export function getHealth(): Promise<HealthResponse> {
  return request<HealthResponse>('/health');
}

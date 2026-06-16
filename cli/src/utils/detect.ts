import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { AIType } from '../types/index.js';

interface DetectionResult {
  detected: AIType[];
  suggested: AIType | null;
}

export function detectAIType(cwd: string = process.cwd()): DetectionResult {
  const detected: AIType[] = [];
  const checks: [string, AIType][] = [
    ['.claude', 'claude'],
    ['.cursor', 'cursor'],
    ['.windsurf', 'windsurf'],
    ['.agents', 'antigravity'],
    ['.agent', 'antigravity'],
    ['.github', 'copilot'],
    ['.kiro', 'kiro'],
    ['.codex', 'codex'],
    ['.roo', 'roocode'],
    ['.qoder', 'qoder'],
    ['.gemini', 'gemini'],
    ['.trae', 'trae'],
    ['.opencode', 'opencode'],
    ['.continue', 'continue'],
    ['.codebuddy', 'codebuddy'],
    ['.factory', 'droid'],
    ['.kilocode', 'kilocode'],
    ['.warp', 'warp'],
    ['.augment', 'augment'],
  ];

  for (const [dir, type] of checks) {
    if (existsSync(join(cwd, dir))) {
      detected.push(type);
    }
  }

  let suggested: AIType | null = null;
  if (detected.length === 1) {
    suggested = detected[0];
  } else if (detected.length > 1) {
    suggested = 'all';
  }

  return { detected, suggested };
}

export function getAITypeDescription(aiType: AIType): string {
  const map: Record<string, string> = {
    claude: 'Claude Code (.claude/skills/)',
    cursor: 'Cursor (.cursor/skills/)',
    windsurf: 'Windsurf (.windsurf/skills/)',
    antigravity: 'Antigravity (.agents/skills/)',
    copilot: 'GitHub Copilot (.github/prompts/)',
    kiro: 'Kiro (.kiro/steering/)',
    codex: 'Codex (.codex/skills/)',
    roocode: 'RooCode (.roo/skills/)',
    qoder: 'Qoder (.qoder/skills/)',
    gemini: 'Gemini CLI (.gemini/skills/)',
    trae: 'Trae (.trae/skills/)',
    opencode: 'OpenCode (.opencode/skills/)',
    continue: 'Continue (.continue/skills/)',
    codebuddy: 'CodeBuddy (.codebuddy/skills/)',
    droid: 'Droid/Factory (.factory/skills/)',
    kilocode: 'KiloCode (.kilocode/skills/)',
    warp: 'Warp (.warp/skills/)',
    augment: 'Augment (.augment/skills/)',
    all: 'All AI assistants',
  };
  return map[aiType] ?? aiType;
}

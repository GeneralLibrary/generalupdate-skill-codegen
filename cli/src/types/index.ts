export type AIType =
  | 'claude' | 'cursor' | 'windsurf' | 'antigravity' | 'copilot'
  | 'kiro' | 'roocode' | 'codex' | 'qoder' | 'gemini'
  | 'trae' | 'opencode' | 'continue' | 'codebuddy' | 'droid'
  | 'kilocode' | 'warp' | 'augment' | 'all';

export interface PlatformConfig {
  platform: string;
  displayName: string;
  folderStructure: {
    root: string;
    skillPath: string;
    filename: string;
  };
  scriptPath: string;
  frontmatter: Record<string, string> | null;
  sections: {
    quickReference: boolean;
  };
  title: string;
  description: string;
  skillOrWorkflow: string;
}

export const AI_TYPES: AIType[] = [
  'claude', 'cursor', 'windsurf', 'antigravity', 'copilot',
  'roocode', 'kiro', 'codex', 'qoder', 'gemini',
  'trae', 'opencode', 'continue', 'codebuddy', 'droid',
  'kilocode', 'warp', 'augment', 'all',
];

export interface Release {
  tag_name: string;
  name: string;
  published_at: string;
  html_url: string;
  assets: Asset[];
}

export interface Asset {
  name: string;
  browser_download_url: string;
  size: number;
}

export const AI_FOLDERS: Record<Exclude<AIType, 'all'>, string[]> = {
  claude: ['.claude'],
  cursor: ['.cursor'],
  windsurf: ['.windsurf'],
  antigravity: ['.agents'],
  copilot: ['.github'],
  kiro: ['.kiro'],
  codex: ['.codex'],
  roocode: ['.roo'],
  qoder: ['.qoder'],
  gemini: ['.gemini'],
  trae: ['.trae'],
  opencode: ['.opencode'],
  continue: ['.continue'],
  codebuddy: ['.codebuddy'],
  droid: ['.factory'],
  kilocode: ['.kilocode'],
  warp: ['.warp'],
  augment: ['.augment'],
};

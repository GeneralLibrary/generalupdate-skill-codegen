import { mkdir, writeFile, cp, access, readdir, stat } from 'node:fs/promises';
import { join, dirname, basename } from 'node:path';
import { homedir } from 'node:os';
import { fileURLToPath } from 'node:url';
import type { PlatformConfig } from '../types/index.js';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ASSETS_DIR = join(__dirname, '..', 'assets');
const SKILL_NAMES = [
  'generalupdate-init',
  'generalupdate-ui',
  'generalupdate-strategy',
  'generalupdate-advanced',
  'generalupdate-troubleshoot',
  'generalupdate-migration',
  'generalupdate-security-audit',
];

const AI_TO_PLATFORM: Record<string, string> = {
  claude: 'claude', cursor: 'cursor', windsurf: 'windsurf',
  antigravity: 'agent', copilot: 'copilot',
  kiro: 'kiro', codex: 'codex', roocode: 'roocode',
  qoder: 'qoder', gemini: 'gemini', trae: 'trae',
  opencode: 'opencode', continue: 'continue', codebuddy: 'codebuddy',
  droid: 'droid', kilocode: 'kilocode', warp: 'warp', augment: 'augment',
};

const AI_ROOT_DIRS: Record<string, string> = {
  claude: '.claude',
  cursor: '.cursor',
  windsurf: '.windsurf',
  antigravity: '.agents',
  copilot: '.github',
  kiro: '.kiro',
  codex: '.codex',
  roocode: '.roo',
  qoder: '.qoder',
  gemini: '.gemini',
  trae: '.trae',
  opencode: '.opencode',
  continue: '.continue',
  codebuddy: '.codebuddy',
  droid: '.factory',
  kilocode: '.kilocode',
  warp: '.warp',
  augment: '.augment',
};

async function exists(path: string): Promise<boolean> {
  try { await access(path); return true; } catch { return false; }
}

/**
 * Install ALL GeneralUpdate skills for a given AI platform.
 * Copies the complete .claude/skills/ directory tree from assets
 * into the target platform's skills directory.
 */
export async function generatePlatformFiles(
  targetDir: string,
  aiType: string,
  isGlobal = false
): Promise<string[]> {
  const effectiveDir = isGlobal ? homedir() : targetDir;
  const rootDir = AI_ROOT_DIRS[aiType];
  if (!rootDir) throw new Error(`Unknown AI type: ${aiType}`);

  const targetSkillsDir = join(effectiveDir, rootDir, 'skills');
  const sourceSkillsDir = join(ASSETS_DIR, 'skills');

  // Ensure target directory exists
  await mkdir(targetSkillsDir, { recursive: true });
  const createdFolders: string[] = [];
  let copiedAny = false;

  for (const skillName of SKILL_NAMES) {
    const src = join(sourceSkillsDir, skillName);
    const dst = join(targetSkillsDir, skillName);

    if (!(await exists(src))) {
      continue; // Skip skills not yet synced
    }

    try {
      await cp(src, dst, { recursive: true, force: true });
      createdFolders.push(`${rootDir}/skills/${skillName}`);
      copiedAny = true;
    } catch {
      // Fallback to shell copy
      const { exec } = await import('node:child_process');
      const { promisify } = await import('node:util');
      const execAsync = promisify(exec);
      try {
        if (process.platform === 'win32') {
          await execAsync(`xcopy "${src}" "${dst}" /E /I /Y`);
        } else {
          await execAsync(`cp -r "${src}/." "${dst}"`);
        }
        createdFolders.push(`${rootDir}/skills/${skillName}`);
        copiedAny = true;
      } catch {
        // Skip individual skill copy failures
      }
    }
  }

  if (!copiedAny) {
    // Fall back to single platform config (legacy)
    const config = await loadPlatformConfig(aiType);
    const skillDir = join(effectiveDir, rootDir, config.folderStructure.skillPath);
    await mkdir(skillDir, { recursive: true });
    const skillContentPath = join(ASSETS_DIR, 'templates', 'base', 'skill-content.md');
    if (await exists(skillContentPath)) {
      const content = await readFile(skillContentPath, 'utf-8');
      const frontmatter = renderFrontmatter(config.frontmatter);
      await writeFile(join(skillDir, config.folderStructure.filename), frontmatter + content, 'utf-8');
    }
    createdFolders.push(rootDir);
  }

  return createdFolders;
}

export async function loadPlatformConfig(aiType: string): Promise<PlatformConfig> {
  const platformName = AI_TO_PLATFORM[aiType];
  if (!platformName) throw new Error(`Unknown AI type: ${aiType}`);
  const configPath = join(ASSETS_DIR, 'templates', 'platforms', `${platformName}.json`);
  const content = await readFile(configPath, 'utf-8');
  return JSON.parse(content) as PlatformConfig;
}

function renderFrontmatter(frontmatter: Record<string, string> | null): string {
  if (!frontmatter) return '';
  const lines = ['---'];
  for (const [key, value] of Object.entries(frontmatter)) {
    if (value.includes(':') || value.includes('"') || value.includes('\n')) {
      lines.push(`${key}: "${value.replace(/"/g, '\\"')}"`);
    } else {
      lines.push(`${key}: ${value}`);
    }
  }
  lines.push('---', '');
  return lines.join('\n');
}

export async function generateAllPlatformFiles(
  targetDir: string,
  isGlobal = false
): Promise<string[]> {
  const allFolders = new Set<string>();
  for (const aiType of Object.keys(AI_TO_PLATFORM)) {
    try {
      const folders = await generatePlatformFiles(targetDir, aiType, isGlobal);
      folders.forEach(f => allFolders.add(f));
    } catch {
      // Skip if generation fails for a platform
    }
  }
  return Array.from(allFolders);
}

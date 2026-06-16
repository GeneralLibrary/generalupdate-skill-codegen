import { mkdir, rm, access, cp, mkdtemp, readdir } from 'node:fs/promises';
import { join, basename } from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { tmpdir } from 'node:os';
import type { AIType } from '../types/index.js';
import { AI_FOLDERS } from '../types/index.js';

const execFileAsync = promisify(execFile);
const EXCLUDED_FILES = ['settings.local.json'];

async function exists(path: string): Promise<boolean> {
  try {
    await access(path);
    return true;
  } catch {
    return false;
  }
}

export async function extractZip(zipPath: string, destDir: string): Promise<void> {
  try {
    const isWindows = process.platform === 'win32';
    if (isWindows) {
      // Use execFile to avoid shell injection
      await execFileAsync('powershell', [
        '-Command',
        `Expand-Archive -Path '${zipPath.replace(/'/g, "''")}' -DestinationPath '${destDir.replace(/'/g, "''")}' -Force`,
      ]);
    } else {
      await execFileAsync('unzip', ['-o', zipPath, '-d', destDir]);
    }
  } catch (error) {
    throw new Error(`Failed to extract zip: ${error}`);
  }
}

export async function copyFolders(
  sourceDir: string,
  targetDir: string,
  aiType: AIType
): Promise<string[]> {
  const copiedFolders: string[] = [];
  const foldersToCopy =
    aiType === 'all'
      ? Object.values(AI_FOLDERS).flat()
      : AI_FOLDERS[aiType];
  const uniqueFolders = [...new Set(foldersToCopy)];

  for (const folder of uniqueFolders) {
    const sourcePath = join(sourceDir, folder);
    const targetPath = join(targetDir, folder);
    const sourceExists = await exists(sourcePath);
    if (!sourceExists) continue;

    await mkdir(targetPath, { recursive: true });

    const filterFn = (src: string): boolean => {
      return !EXCLUDED_FILES.includes(basename(src));
    };

    try {
      await cp(sourcePath, targetPath, { recursive: true, filter: filterFn });
      copiedFolders.push(folder);
    } catch {
      // Fallback to shell copy for older Node — use execFile to avoid injection
      try {
        if (process.platform === 'win32') {
          await execFileAsync('xcopy', [sourcePath, targetPath, '/E', '/I', '/Y']);
        } else {
          await execFileAsync('cp', ['-r', `${sourcePath}/.`, targetPath]);
        }
        copiedFolders.push(folder);
      } catch {
        // Skip if copy fails
      }
    }
  }

  return copiedFolders;
}

export async function cleanup(tempDir: string): Promise<void> {
  try {
    await rm(tempDir, { recursive: true, force: true });
  } catch {
    // Ignore cleanup errors
  }
}

export async function createTempDir(): Promise<string> {
  return mkdtemp(join(tmpdir(), 'gskill-'));
}

async function findExtractedRoot(tempDir: string): Promise<string> {
  const entries = await readdir(tempDir, { withFileTypes: true });
  const dirs = entries.filter((e) => e.isDirectory());
  if (dirs.length === 1) {
    return join(tempDir, dirs[0].name);
  }
  return tempDir;
}

export async function installFromZip(
  zipPath: string,
  targetDir: string,
  aiType: AIType
): Promise<{ copiedFolders: string[]; tempDir: string }> {
  const tempDir = await createTempDir();
  try {
    await extractZip(zipPath, tempDir);
    const extractedRoot = await findExtractedRoot(tempDir);
    const copiedFolders = await copyFolders(extractedRoot, targetDir, aiType);
    return { copiedFolders, tempDir };
  } catch (error) {
    await cleanup(tempDir);
    throw error;
  }
}

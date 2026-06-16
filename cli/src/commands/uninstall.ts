import { rm, stat } from 'node:fs/promises';
import { join } from 'node:path';
import { homedir } from 'node:os';
import chalk from 'chalk';
import ora from 'ora';
import prompts from 'prompts';
import type { AIType } from '../types/index.js';
import { AI_TYPES, AI_FOLDERS } from '../types/index.js';
import { detectAIType, getAITypeDescription } from '../utils/detect.js';

interface UninstallOptions {
  ai?: AIType;
  global?: boolean;
}

const SKILL_NAMES = [
  'generalupdate-init', 'generalupdate-ui', 'generalupdate-strategy',
  'generalupdate-advanced', 'generalupdate-troubleshoot',
  'generalupdate-migration', 'generalupdate-security-audit',
];

async function removeSkillDir(baseDir: string, aiType: Exclude<AIType, 'all'>): Promise<string[]> {
  const folders = AI_FOLDERS[aiType];
  const removed: string[] = [];

  for (const folder of folders) {
    for (const skillName of SKILL_NAMES) {
      const skillDir = join(baseDir, folder, 'skills', skillName);
      try {
        await stat(skillDir);
        await rm(skillDir, { recursive: true, force: true });
        removed.push(`${folder}/skills/${skillName}`);
      } catch (err: unknown) {
        if ((err as NodeJS.ErrnoException).code !== 'ENOENT') throw err;
      }
    }
  }

  return removed;
}

export async function uninstallCommand(options: UninstallOptions): Promise<void> {
  console.log(chalk.bold('\n🗑️  GeneralUpdate Skill Uninstaller\n'));

  const isGlobal = !!options.global;
  const baseDir = isGlobal ? homedir() : process.cwd();
  const locationLabel = isGlobal ? '~/ (global)' : process.cwd();

  let aiType = options.ai;
  const { detected: initialDetected } = detectAIType(baseDir);

  if (!aiType) {
    if (initialDetected.length === 0) {
      console.log(chalk.yellow('No installed skill directories detected.'));
      return;
    }

    console.log(`Detected installations: ${initialDetected.map(t => chalk.cyan(t)).join(', ')}`);

    const choices = [
      ...initialDetected.map(type => ({
        title: getAITypeDescription(type),
        value: type,
      })),
      { title: 'All detected', value: 'all' as AIType },
    ];

    const response = await prompts({
      type: 'select',
      name: 'aiType',
      message: 'Select which AI skill to uninstall:',
      choices,
    });

    if (!response.aiType) {
      console.log(chalk.yellow('Uninstall cancelled'));
      return;
    }
    aiType = response.aiType as AIType;
  }

  const { confirmed } = await prompts({
    type: 'confirm',
    name: 'confirmed',
    message: `Remove GeneralUpdate skill for ${chalk.cyan(getAITypeDescription(aiType))} from ${locationLabel}?`,
    initial: false,
  });

  if (!confirmed) {
    console.log(chalk.yellow('Uninstall cancelled'));
    return;
  }

  const spinner = ora('Removing skill files...').start();

  try {
    const allRemoved: string[] = [];

    if (aiType === 'all') {
      for (const type of initialDetected) {
        const removed = await removeSkillDir(baseDir, type as Exclude<AIType, 'all'>);
        allRemoved.push(...removed);
      }
    } else {
      const removed = await removeSkillDir(baseDir, aiType as Exclude<AIType, 'all'>);
      allRemoved.push(...removed);
    }

    if (allRemoved.length === 0) {
      spinner.warn('No skill files found to remove');
      return;
    }

    spinner.succeed('Skill files removed!');

    console.log();
    console.log(chalk.bold('Removed:'));
    allRemoved.forEach(folder => {
      console.log(`  ${chalk.red('-')} ${folder}`);
    });

    console.log();
    console.log(chalk.green('Uninstalled successfully!'));
  } catch (error) {
    spinner.fail('Uninstall failed');
    if (error instanceof Error) {
      console.error(chalk.red(error.message));
    }
    process.exit(1);
  }
}

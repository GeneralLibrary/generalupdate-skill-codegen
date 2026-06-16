import { join } from 'node:path';
import { homedir } from 'node:os';
import chalk from 'chalk';
import ora from 'ora';
import prompts from 'prompts';
import type { AIType } from '../types/index.js';
import { AI_TYPES } from '../types/index.js';
import { generatePlatformFiles } from '../utils/template.js';
import { detectAIType, getAITypeDescription } from '../utils/detect.js';

interface InitOptions {
  ai?: AIType;
  force?: boolean;
  global?: boolean;
  generate?: boolean;
  framework?: string;
  strategy?: string;
}

export async function initCommand(options: InitOptions): Promise<void> {
  console.log(chalk.bold('\n🔧 GeneralUpdate Skill Installer\n'));

  let aiType = options.ai;

  if (!aiType) {
    const { detected, suggested } = detectAIType();

    if (detected.length > 0) {
      console.log(`Detected: ${detected.map(t => chalk.cyan(t)).join(', ')}`);
    }

    const response = await prompts({
      type: 'select',
      name: 'aiType',
      message: 'Select AI assistant to install for:',
      choices: AI_TYPES.map(type => ({
        title: getAITypeDescription(type),
        value: type,
      })),
      initial: suggested ? AI_TYPES.indexOf(suggested) : 0,
    });

    if (!response.aiType) {
      console.log(chalk.yellow('Installation cancelled'));
      return;
    }
    aiType = response.aiType as AIType;
  }

  const isGlobal = !!options.global;
  const modeLabel = isGlobal ? ' (global)' : '';
  console.log(`Installing for: ${chalk.cyan(getAITypeDescription(aiType))}${modeLabel}`);

  const spinner = ora('Installing files...').start();
  const cwd = process.cwd();

  try {
    const effectiveDir = isGlobal ? homedir() : cwd;
    const copiedFolders = await generatePlatformFiles(effectiveDir, aiType, isGlobal);

    spinner.succeed('Skill files installed!');

    console.log();
    console.log(chalk.bold('Installed folders:'));
    copiedFolders.forEach(folder => {
      console.log(`  ${chalk.green('+')} ${folder}/`);
    });

    console.log();
    console.log(chalk.bold('Next steps:'));
    console.log(chalk.dim('  1. Restart your AI coding assistant'));
    console.log(chalk.dim('  2. Try: "给我的 WPF 应用添加自动更新"'));
    console.log(chalk.dim('  3. Or: "配置 OSS 静默更新"'));

    // Run code generator if requested
    if (options.generate) {
      console.log();
      const genResponse = await prompts({
        type: 'confirm',
        name: 'run',
        message: 'Run code generator now?',
        initial: true,
      });
      if (genResponse.run) {
        const { generateCommand } = await import('./generate.js');
        await generateCommand({
          framework: options.framework || 'wpf-原生',
          strategy: options.strategy || 'standard',
          bowl: false,
          projectName: 'MyApp',
          output: './Generated',
        });
      }
    }

    console.log();
  } catch (error) {
    spinner.fail('Installation failed');
    if (error instanceof Error) {
      console.error(chalk.red(error.message));
    }
    process.exit(1);
  }
}

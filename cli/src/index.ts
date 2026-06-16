import { readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import { Command } from 'commander';
import { initCommand } from './commands/init.js';
import { uninstallCommand } from './commands/uninstall.js';
import { generateCommand } from './commands/generate.js';
import { versionsCommand } from './commands/versions.js';
import { updateCommand } from './commands/update.js';
import type { AIType } from './types/index.js';
import { AI_TYPES } from './types/index.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const pkg = JSON.parse(readFileSync(join(__dirname, '../package.json'), 'utf-8'));

const program = new Command();

program
  .name('gskill')
  .description('CLI to install GeneralUpdate skill for AI coding assistants')
  .version(pkg.version);

program
  .command('init')
  .description('Install GeneralUpdate skill to current project')
  .option('-a, --ai <type>', `AI assistant type (${AI_TYPES.join(', ')})`)
  .option('-f, --force', 'Overwrite existing files')
  .option('-g, --global', 'Install globally to home directory (~/) instead of current project')
  .option('--generate', 'Also run code generator after installation')
  .option('--framework <framework>', 'Target UI framework for code generation')
  .option('--strategy <strategy>', 'Update strategy for code generation')
  .action(async (options) => {
    if (options.ai && !AI_TYPES.includes(options.ai)) {
      console.error(`Invalid AI type: ${options.ai}`);
      console.error(`Valid types: ${AI_TYPES.join(', ')}`);
      process.exit(1);
    }
    await initCommand({
      ai: options.ai as AIType | undefined,
      force: options.force,
      global: options.global,
      generate: options.generate,
      framework: options.framework,
      strategy: options.strategy,
    });
  });

program
  .command('uninstall')
  .description('Remove GeneralUpdate skill from current project or globally')
  .option('-a, --ai <type>', `AI assistant type (${AI_TYPES.join(', ')})`)
  .option('-g, --global', 'Uninstall from home directory (~/) instead of current project')
  .action(async (options) => {
    if (options.ai && !AI_TYPES.includes(options.ai)) {
      console.error(`Invalid AI type: ${options.ai}`);
      console.error(`Valid types: ${AI_TYPES.join(', ')}`);
      process.exit(1);
    }
    await uninstallCommand({
      ai: options.ai as AIType | undefined,
      global: options.global,
    });
  });

program
  .command('generate')
  .description('Run code generator (Python)')
  .option('-f, --framework <framework>', 'Target UI framework')
  .option('-s, --strategy <strategy>', 'Update strategy')
  .option('--bowl', 'Include Bowl crash daemon')
  .option('-n, --project-name <name>', 'Project name')
  .option('-o, --output <dir>', 'Output directory', './Generated')
  .action(async (options) => {
    await generateCommand(options);
  });

program
  .command('versions')
  .description('List available versions from GitHub')
  .action(versionsCommand);

program
  .command('update')
  .description('Update GeneralUpdate skill to latest version')
  .option('-a, --ai <type>', `AI assistant type (${AI_TYPES.join(', ')})`)
  .action(async (options) => {
    if (options.ai && !AI_TYPES.includes(options.ai)) {
      console.error(`Invalid AI type: ${options.ai}`);
      console.error(`Valid types: ${AI_TYPES.join(', ')}`);
      process.exit(1);
    }
    await updateCommand({ ai: options.ai as AIType | undefined });
  });

program.parse();

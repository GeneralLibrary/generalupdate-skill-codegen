import chalk from 'chalk';
import { execSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

interface GenerateOptions {
  framework: string;
  strategy: string;
  bowl?: boolean;
  projectName?: string;
  output?: string;
  version?: string;
  updateUrl?: string;
}

export async function generateCommand(options: GenerateOptions): Promise<void> {
  console.log(chalk.bold('\n⚙️  GeneralUpdate Code Generator\n'));

  // Find the generate.py script
  const possiblePaths = [
    join(process.cwd(), '.claude', 'scripts', 'generate.py'),
    join(process.cwd(), '.claude', 'skills', 'generalupdate-init', 'scripts', 'generate.py'),
  ];

  let scriptPath = '';
  for (const p of possiblePaths) {
    if (existsSync(p)) {
      scriptPath = p;
      break;
    }
  }

  if (!scriptPath) {
    console.log(chalk.yellow('⚠️  Code generator script not found.'));
    console.log(chalk.dim('   Install with: gskill init'));
    console.log(chalk.dim('   Then run: python3 .claude/scripts/generate.py --help'));
    return;
  }

  const args = [
    `--framework "${options.framework}"`,
    `--strategy "${options.strategy}"`,
    options.bowl ? '--bowl' : '',
    options.projectName ? `--project-name "${options.projectName}"` : '',
    options.output ? `--output "${options.output}"` : '',
    options.version ? `--version "${options.version}"` : '',
    options.updateUrl ? `--update-url "${options.updateUrl}"` : '',
  ].filter(Boolean).join(' ');

  const cmd = `python3 "${scriptPath}" ${args}`;
  console.log(chalk.dim(`Running: ${cmd}\n`));

  try {
    execSync(cmd, { stdio: 'inherit' });
  } catch (error) {
    console.error(chalk.red('Code generation failed. Is Python 3 installed?'));
    process.exit(1);
  }
}

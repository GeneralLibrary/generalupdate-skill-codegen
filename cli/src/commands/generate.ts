import chalk from 'chalk';
import { spawnSync } from 'node:child_process';
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

  // Build argument array (no string concatenation — prevents injection)
  const args = [
    '--framework', options.framework,
    '--strategy', options.strategy,
  ];
  if (options.bowl) args.push('--bowl');
  if (options.projectName) args.push('--project-name', options.projectName);
  if (options.output) args.push('--output', options.output);
  if (options.version) args.push('--version', options.version);
  if (options.updateUrl) args.push('--update-url', options.updateUrl);

  console.log(chalk.dim(`Running: python3 "${scriptPath}" ${args.join(' ')}\n`));

  const result = spawnSync('python3', [scriptPath, ...args], {
    stdio: 'inherit',
    shell: false,
  });

  if (result.error || result.status !== 0) {
    console.error(chalk.red('Code generation failed. Is Python 3 installed?'));
    process.exit(1);
  }
}

import { cpSync, existsSync, mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = dirname(fileURLToPath(import.meta.url));
const projectRoot = join(root, '..');
const standaloneRoot = join(projectRoot, '.next', 'standalone');
const standaloneNextRoot = join(standaloneRoot, '.next');
const nextStaticSource = join(projectRoot, '.next', 'static');
const nextStaticTarget = join(standaloneNextRoot, 'static');
const publicSource = join(projectRoot, 'public');
const publicTarget = join(standaloneRoot, 'public');

if (!existsSync(standaloneRoot)) {
  console.warn('Standalone output not found. Skipping static asset copy.');
  process.exit(0);
}

mkdirSync(standaloneNextRoot, { recursive: true });

if (existsSync(nextStaticSource)) {
  cpSync(nextStaticSource, nextStaticTarget, { recursive: true, force: true });
}

if (existsSync(publicSource)) {
  cpSync(publicSource, publicTarget, { recursive: true, force: true });
}

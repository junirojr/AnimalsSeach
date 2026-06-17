import nextJest from "next/jest.js";
import type { Config } from "jest";

const criarConfigJest = nextJest({ dir: "./" });

const config: Config = {
  testEnvironment: "jest-environment-jsdom",
  // Forca a resolucao dos pacotes (msw/@mswjs/interceptors) para o build
  // CJS/node em vez do build ESM de navegador.
  testEnvironmentOptions: {
    customExportConditions: [""],
  },
  setupFiles: ["<rootDir>/jest.polyfills.ts"],
  setupFilesAfterEnv: ["<rootDir>/jest.setup.ts"],
  moduleNameMapper: {
    "^@/(.*)$": "<rootDir>/src/$1",
  },
  testMatch: ["**/*.test.ts?(x)"],
};

// O next/jest sobrescreve transformIgnorePatterns. O MSW v2 e suas
// dependencias sao ESM puro e precisam ser transformados, entao
// pos-processamos a config gerada para reaplicar o nosso padrao depois.
const pacotesEsm = [
  "msw",
  "@mswjs",
  "@bundled-es-modules",
  "until-async",
  "@open-draft",
  "outvariant",
  "strict-event-emitter",
  "headers-polyfill",
  "rettime",
  "is-node-process",
].join("|");

export default async (): Promise<Config> => {
  const configResolvida = await criarConfigJest(config)();

  configResolvida.transformIgnorePatterns = [
    `/node_modules/(?!(?:.pnpm/)?(?:${pacotesEsm})/)`,
    "^.+\\.module\\.(css|sass|scss)$",
  ];

  return configResolvida;
};

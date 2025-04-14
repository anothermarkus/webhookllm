const { RuleTester } = require("eslint");
const rule = require("../lib/rules/too-many-injected-services");

const ruleTester = new RuleTester({
  parser: require.resolve("@typescript-eslint/parser"),
  parserOptions: {
    ecmaVersion: 2020,
    sourceType: "module",
  },
});

ruleTester.run("limit-injected-services", rule, {
  valid: [
    {
      code: `
        class MyComponent {
          constructor(private serviceA: A, private serviceB: B) {}
        }
      `,
      options: [{ max: 3 }],
    },
    {
      code: `
        class MyComponent {
          constructor(private serviceA: A, private serviceB: B, private serviceC: C, private serviceD: D) {}
        }
      `,
      options: [{ max: 4 }],
    },
  ],

  invalid: [
    {
      code: `
        class MyComponent {
          constructor(
            private a: A,
            private b: B,
            private c: C,
            private d: D,
            private e: E,
            private f: F
          ) {}
        }
      `,
      options: [{ max: 5 }],
      errors: [
        {
          messageId: "tooManyServices",
          data: {
            count: 6,
            max: 5,
          },
        },
      ],
    },
  ],
});

console.log("✅ limit-injected-services test passed");

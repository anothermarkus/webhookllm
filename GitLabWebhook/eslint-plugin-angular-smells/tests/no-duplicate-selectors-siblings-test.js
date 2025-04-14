const { RuleTester } = require("eslint");
const rule = require("../lib/rules/no-duplicate-selectors-siblings"); // Adjust path as needed

const ruleTester = new RuleTester({
  parser: require.resolve("@angular-eslint/template-parser"),
  parserOptions: {
    ecmaVersion: 2020,
  },
});

ruleTester.run("no-duplicate-custom-components", rule, {
  valid: [
    {
      code: `<my-component></my-component><another-component></another-component>`,
    },
    {
      code: `<my-component *ngFor="let item of items"></my-component>`, // Acceptable use of repetition
    },
  ],
  invalid: [
    {
      code: `<my-component></my-component><my-component></my-component>`,
      errors: [
        {
          message: `The custom component "my-component" is used multiple times at the same level. Consider using "ngFor" to iterate over the component instances.`,
        },
      ],
    },
  ],
});

console.log("✅ no-duplicate-selectors-siblings test passed");
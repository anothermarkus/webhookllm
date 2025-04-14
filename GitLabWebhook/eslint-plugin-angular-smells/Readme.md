# ESLint Plugin for Angular Smells

This ESLint plugin provides custom linting rules to detect common code smells and bad practices in Angular applications. The goal is to help developers maintain cleaner, more maintainable Angular code by identifying potential issues early in the development process.
Source: https://typescript-eslint.io/developers/custom-rules

```
>npx eslint .\codesmellsamples\*         

...\codesmellsamplestest.html
  11:21  warning  The custom component "app-lotta-selectors" is used multiple times at the same level. Consider using "ngFor" to iterate over the component instances  angular-smells/no-duplicate-selectors-siblings

..\codesmellsamples\test.ts
  3:5  warning  Constructor has 6 injected services (limit is 1)  angular-smells/too-many-injected-services

✖ 2 problems (0 errors, 2 warnings)
```


## Installation

To install and use the `eslint-plugin-angular-smells` plugin, follow the steps below:

### 1. Install ESLint

If you haven’t installed ESLint in your Angular project yet, install it by running the following command:

```bash
npm install eslint --save-dev
```

### 2. Install the Plugin

To install the `eslint-plugin-angular-smells` plugin, run the following command:

```bash
npm install @anothermarcus-org/eslint-plugin-angular-smells --save-dev
```

This command installs the plugin and adds it to your project as a **development dependency**.

### 3. Configure ESLint

In your `.eslintrc.js` (or `.eslintrc.json`), add the plugin to the `plugins` section and enable any rules you want to use.

Here’s an example of what your `.eslintrc.js` might look like:

```javascript
module.exports = {
  extends: [
    'eslint:recommended',
    'plugin:@angular-eslint/recommended',
  ],
  plugins: ['angular-smells'],
  rules: {
    'angular-smells/avoid-long-methods': 'warn',         // Example rule
    'angular-smells/no-hardcoded-localhost': 'warn',    // Example rule
    // Add more rules as needed
  },
};
```

This configuration:
- Adds the `angular-smells` plugin to your project.
- Configures two example rules:
  - **`avoid-long-methods`**: Warns if methods exceed a certain length (e.g., 50 lines).
  - **`no-hardcoded-localhost`**: Warns if `localhost` URLs are hardcoded in your codebase.

### 4. Running ESLint

Once the plugin is installed and configured, you can run ESLint to check your code for rule violations.

To run ESLint on your Angular project, use the following command:

```bash
npx eslint . --ext .ts,.html
```

This command will check all `.ts` (TypeScript) and `.html` files in your project for linting issues based on the rules you've configured.

---

## Rules

### 1. `angular-smells/avoid-long-methods`

This rule checks if a method exceeds a certain length (e.g., 50 lines). It encourages developers to split long methods into smaller, more manageable pieces of code. You can customize the length threshold based on your needs.

#### Example:
```js
function longMethod() {
  // This method exceeds the allowed length and should be refactored.
}
```

### 2. `angular-smells/no-hardcoded-localhost`

This rule detects hardcoded `localhost` URLs in your codebase. It's essential to avoid using `localhost` directly, as it can cause issues when your code is deployed to different environments.

#### Example:
```js
const apiUrl = 'http://localhost:3000/api';  // This should be avoided
```

### 3. `angular-smells/no-duplicate-selectors`

This rule detects when multiple CSS selectors are defined at the same level, which may cause duplication and unnecessary complexity in the stylesheets. It's important to keep selectors organized and avoid redundant definitions.

#### Example:
```css
/* Duplicated selector */
.button { background: red; }
.button { color: white; }
```

---

## Development

If you'd like to contribute to this plugin, follow these steps to set up the development environment:

### 1. Clone the repository

First, clone this repository to your local machine:

```bash
git clone https://github.com/yourusername/eslint-plugin-angular-smells.git
cd eslint-plugin-angular-smells
```

### 2. Install dependencies

Install the required dependencies:

```bash
npm install
```

### 3. Run tests

If you have tests set up for the plugin, run them using:

```bash
npm test
```

### 4. Make your changes

Feel free to modify the code and add new features or rules to the plugin. When you’re ready to test your changes, make sure to run ESLint on your codebase and check if your new rules work as expected.

---

## License

This plugin is licensed under the ISC License. See the [LICENSE](./LICENSE) file for more information.

---

### Summary:

- Install ESLint and the plugin: `npm install eslint @anothermarcus-org/eslint-plugin-angular-smells --save-dev`.
- Configure the plugin in `.eslintrc.js`.
- Run ESLint on your project: `npx eslint . --ext .ts,.html`.
- The plugin includes rules for **long methods**, **hardcoded localhost URLs**, and **duplicated selectors**.

module.exports = {
    parser: '@typescript-eslint/parser',
    plugins: ['angular-smells'],
    overrides: [
      {
        files: ['*.ts'],
        rules: {
          'angular-smells/too-many-injected-services': ['warn', { max: 6 }],
        }
      },
      {
        files: ['*.html'],
        parser: '@angular-eslint/template-parser',
        rules: {
          'angular-smells/no-duplicate-selectors-siblings': 'warn'
        }
      }
    ]
  };
  
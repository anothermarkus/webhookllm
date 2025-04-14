module.exports = {
    rules: {
      'too-many-injected-services': require('./lib/rules/too-many-injected-services'),
      'no-duplicate-selectors-siblings': require('./lib/rules/no-duplicate-selectors-siblings')
    }
  };
  
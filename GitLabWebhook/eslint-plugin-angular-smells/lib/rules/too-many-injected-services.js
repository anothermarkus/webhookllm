module.exports = {
    meta: {
      type: 'suggestion',
      docs: {
        description: 'Limit the number of services injected into an Angular component',
      },
      schema: [
        {
          type: 'object',
          properties: {
            max: {
              type: 'number',
              default: 5
            }
          },
          additionalProperties: false
        }
      ],
      messages: {
        tooManyServices: 'Constructor has {{count}} injected services (limit is {{max}}).'
      }
    },
  
    create(context) {
      const max = context.options[0]?.max || 5;
  
      return {
        MethodDefinition(node) {
          if (node.kind !== 'constructor') return;
  
          const paramCount = node.value.params.length;
          if (paramCount > max) {
            context.report({
              node,
              messageId: 'tooManyServices',
              data: {
                count: paramCount,
                max: max
              }
            });
          }
        }
      };
    }
  };
  
using Transformalize.Configuration;
using Transformalize.Core.Entity_;
using Transformalize.Core.Process_;

namespace Transformalize.Core.Field_
{
    public class FieldReader : IFieldReader
    {
        private readonly Entity _entity;

        public FieldReader(Entity entity)
        {
            _entity = entity ?? new Entity();
        }

        public Field Read(FieldConfigurationElement field, FieldType fieldType = FieldType.Field)
        {
            var f = new Field(field.Type, field.Length, fieldType, field.Output, field.Default)
                        {
                            Process = Process.Name,
                            Entity = _entity.Name,
                            Schema = _entity.Schema,
                            Name = field.Name,
                            Alias = field.Alias,
                            Precision = field.Precision,
                            Scale = field.Scale,
                            Input = field.Input,
                            Unicode = field.Unicode,
                            VariableLength = field.VariableLength,
                            Aggregate = field.Aggregate.ToLower(),
                        };
            f.Transforms = new FieldTransformReader(f, field.Transforms).Read();
            return f;
        }
    }
}
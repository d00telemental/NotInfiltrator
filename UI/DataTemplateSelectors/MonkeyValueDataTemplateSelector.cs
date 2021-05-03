using NotInfiltrator.Serialization.Monkey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NotInfiltrator.UI.DataTemplateSelectors
{
    public class MonkeyValueDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element != null && item != null)
            {
                return item switch
                {
                    StructObjectField { MetaData: { Type: FieldType.Int8 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt8 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int16 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt16 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int32 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt32 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Boolean } } => element.FindResource("BooleanFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Float } } => element.FindResource("FloatFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Double } } => element.FindResource("FloatFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.String } } => element.FindResource("StringValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Reference } } => element.FindResource("ReferenceFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.InlineStruct } } => element.FindResource("InlineStructFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Array } } => element.FindResource("ArrayValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Enum } } => element.FindResource("EnumValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Symbol } } => element.FindResource("SymbolValue") as DataTemplate,
                    ArrayObject => element.FindResource("ArrayValue") as HierarchicalDataTemplate,

                    ReferenceValue rv when rv.Value is StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is UnstructuredObject => element.FindResource("UnstructuredObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is ArrayObject => element.FindResource("ArrayObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue { IsNil: true } => element.FindResource("NilReferenceValue") as DataTemplate,

                    StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,

                    UnstructuredObjectEntry { Type: FieldType.Int8 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt8 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int16 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt16 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int32 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt32 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int64 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt64 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Boolean } => element.FindResource("UnstructuredObjectBooleanValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Float } => element.FindResource("UnstructuredObjectFloatValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Double } => element.FindResource("UnstructuredObjectFloatValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.String } => element.FindResource("UnstructuredObjectStringValue") as DataTemplate,

                    UnstructuredObjectEntry => element.FindResource("UnstructuredObjectEntryValue") as HierarchicalDataTemplate,

                    Int8Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt8Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int16Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt16Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int32Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt32Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int64Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt64Value => element.FindResource("IntegerValue") as DataTemplate,
                    BooleanValue => element.FindResource("BooleanValue") as DataTemplate,
                    FloatValue => element.FindResource("FloatValue") as DataTemplate,
                    DoubleValue => element.FindResource("FloatValue") as DataTemplate,
                    //StringValue => element.FindResource("StringValue") as DataTemplate,
                    InlineStructValue => element.FindResource("InlineStructValue") as HierarchicalDataTemplate,

                    _ => element.FindResource("OtherValue") as DataTemplate
                };
            }
            return null;
        }
    }
}

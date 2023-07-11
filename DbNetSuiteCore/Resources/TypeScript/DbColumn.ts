class DbColumn {
    columnExpression?: string;
    columnName?: string;
    columnKey?: string;
    label?: string;
    format?: string;
    lookup?: string;
    style?: string;
    unmatched = false;
    foreignKey?: boolean;
    foreignKeyValue?: object | string;
    display?: boolean;
    primaryKey?: boolean;
    index?: number;
    dataType?: string;
}
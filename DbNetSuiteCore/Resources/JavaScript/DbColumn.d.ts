declare class DbColumn {
    columnExpression?: string;
    columnName?: string;
    columnKey?: string;
    dataType?: string;
    display?: boolean;
    foreignKey?: boolean;
    foreignKeyValue?: object | string;
    format?: string;
    index?: number;
    label?: string;
    lookup?: string;
    primaryKey?: boolean;
    columnSize?: number;
    style?: string;
    unmatched: boolean;
}

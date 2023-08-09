class DbColumn {
    browse?: boolean;
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
    search?: boolean;
    style?: string;
    unmatched = false;
    download?: boolean;
    image?: boolean;
    extension?: string;
    uploadMetaData?: string;
    uploadMetaDataColumn?: string;
}
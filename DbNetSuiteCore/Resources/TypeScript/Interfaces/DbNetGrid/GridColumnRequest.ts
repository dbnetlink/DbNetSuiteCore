interface GridColumnRequest
{
    columnExpression?: string;
    columnName?: string;
    label?: string;
    format?: string;
    lookup?: string;
    style?: string;
    unmatched: boolean;
    foreignKey?: boolean;
    foreignKeyValue?: object;
    display?: boolean;
    filter?: boolean;
    dataType?: string;
    dataOnly?: boolean;
    download?: boolean;
    image?: boolean;
    extension?: string;
    uploadMetaData: string;
    uploadMetaDataColumn: string;
}
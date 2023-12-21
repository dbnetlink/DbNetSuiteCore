interface GridColumnRequest {
    browse?: boolean;
    columnExpression?: string;
    columnName?: string;
    label?: string;
    format?: string;
    lookup?: string;
    lookupDataTable?: object;
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

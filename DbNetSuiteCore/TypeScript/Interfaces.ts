interface DbNetGridConfiguration {
    id: string;
    connectionString: string;
    tableName: string;
    pageSize: number;
    currentPage: number;
    totalPages: number;
    columns: DbColumn[];
    html: any;
    searchToken: string;
}

interface Column {
    columnKey: string;
    columnName: string;
    label: string;
}

interface DbColumn extends Column {
    autoIncrement: boolean;
    isBoolean: boolean;
    baseSchemaName: string;
    baseTableName: string;
    bulkInsert: boolean;
    columnExpression: string;
    columnExpressionKey: string;
    columnSize: number;
    culture: string;
    dataType: string;
    dbDataType: string;
    display: boolean;
    editDisplay: boolean;
    updateReadOnly: boolean;
    foreignKey: boolean;
    format: string;
    searchFormat: string;
    initialValue: string;
    insertReadOnly: boolean;
    lookup: string;
    lookupDataType: string;
    lookupTable: string;
    lookupTextField: string;
    lookupTextExpression: string;
    lookupValueField: string;
    maxThumbnailHeight: number;
    placeHolder: string;
    primaryKey: boolean;
    readOnly: boolean;
    required: boolean;
    simpleSearch: boolean;
    search: boolean;
    searchLookup: string;
    searchColumnOrderSearch: number;
    sequenceName: string;
    style: string;
    toolTip: string;
    unique: boolean;
    uploadDataColumn: string;
    uploadExtFilter: string;
    uploadFileNameColumn: string;
    uploadMaxFileSize: number;
    uploadOverwrite: boolean;
    uploadRename: boolean;
    uploadRootFolder: string;
    uploadSubFolder: string;
    xmlAttributeName: string;
    xmlElementName: string;
    tableName: string;
    parentColumnIndex: number;
}
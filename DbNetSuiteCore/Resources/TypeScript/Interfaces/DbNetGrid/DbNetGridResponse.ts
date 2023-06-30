interface DbNetGridResponse extends DbNetSuiteResponse {
    toolbar: string;
    data: string;
    currentPage: number;
    totalPages: number;
    totalRows: number;
    columns?: GridColumn[];
    record?: Dictionary<string>;
    searchParams?: Array<SearchParam>;
}


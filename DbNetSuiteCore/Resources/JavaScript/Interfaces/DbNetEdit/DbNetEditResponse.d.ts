interface DbNetEditResponse extends DbNetGridEditResponse {
    form: string;
    currentRow: number;
    columns?: EditColumn[];
    primaryKey?: string;
}

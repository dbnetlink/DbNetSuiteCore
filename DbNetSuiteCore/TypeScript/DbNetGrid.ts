/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />

interface IDbNetGridConfiguration {
    ConnectionString: string;
    TableName: string;
}

class DbNetGrid extends Ajax {
    configuration: IDbNetGridConfiguration;

    constructor(configuration : IDbNetGridConfiguration) {
        super();
        this.configuration = configuration;
        this.init();
    }

    private init() {
        this.callServer("Init", this.configuration, (response) => { this.initCallback(response) });
    }

    private initCallback(response: any) {
        alert(response)
    }
}
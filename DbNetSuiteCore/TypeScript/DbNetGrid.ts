/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />


class DbNetGrid extends Ajax {
    constructor() {
        super();
        this.init();
    }

    private init() {
        let params = {};
        this.callServer("Init", params, (response) => { this.initCallback(response) });
    }

    private initCallback(response: any) {
        alert(response)
    }
}
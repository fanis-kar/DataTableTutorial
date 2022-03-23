$(document).ready(function () {
    LoadData();

    var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();
    connection.start();

    connection.on("LoadDataTrigger", function () {
        LoadData();
    })

    //LoadData();

    //------------------------------------------------------------------------------------------------//

    function LoadData() {

        var tbl = $("#tbl").DataTable({
            "language": {
                "url": "//cdn.datatables.net/plug-ins/9dcbecd42ad/i18n/Greek.json"
            },
            "dom": 'Bfrtip',
            "buttons": [{
                "extend": 'copy',
                "text": 'Αντιγραφή',
                "titleAttr": 'Copy',
                "action": exportaction
            },
            {
                "extend": 'excel',
                "text": 'Εξαγωγή σε Excel',
                "titleAttr": 'Excel',
                "action": exportaction
            },
            {
                "extend": 'csv',
                "text": 'Εξαγωγή σε CSV',
                "titleAttr": 'CSV',
                "action": exportaction
            },
            {
                "extend": 'pdf',
                "text": 'Εξαγωγή σε PDF',
                "titleAttr": 'PDF',
                "action": exportaction
            },
            {
                "extend": 'print',
                "text": 'Εκτύπωση',
                "titleAttr": 'Print',
                "action": exportaction
            }],
            "processing": true,
            "paging": true,
            "searching": true,
            "filter": true,
            "serverSide": true,
            "destroy": true,
            "ajax": {
                //"url": "/api/data",
                "url": "/Home/GetAll",
                "type": "POST",
                "datatype": "json"
            },
            "columnDefs": [{
                //"targets": [0],
                "visible": false,
                "searchable": false
            }],
            "columns": [
                { "data": "Id", "name": "Id", "autoWidth": true },
                { "data": "Name", "name": "Name", "autoWidth": true },
                { "data": "Status", "name": "Status", "autoWidth": true },
            ]
        });
    }



    //------------------------------------------------------------------------------------------------//

    function exportaction(e, dt, button, config) {
        var self = this;
        var oldStart = dt.settings()[0]._iDisplayStart;
        dt.one('preXhr', function (e, s, data) {
            // Just this once, load all data from the server...
            data.start = 0;
            data.length = 2147483647;
            dt.one('preDraw', function (e, settings) {
                // Call the original action function
                if (button[0].className.indexOf('buttons-copy') >= 0) {
                    $.fn.dataTable.ext.buttons.copyHtml5.action.call(self, e, dt, button, config);
                } else if (button[0].className.indexOf('buttons-excel') >= 0) {
                    $.fn.dataTable.ext.buttons.excelHtml5.available(dt, config) ?
                        $.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config) :
                        $.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
                } else if (button[0].className.indexOf('buttons-csv') >= 0) {
                    $.fn.dataTable.ext.buttons.csvHtml5.available(dt, config) ?
                        $.fn.dataTable.ext.buttons.csvHtml5.action.call(self, e, dt, button, config) :
                        $.fn.dataTable.ext.buttons.csvFlash.action.call(self, e, dt, button, config);
                } else if (button[0].className.indexOf('buttons-pdf') >= 0) {
                    $.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config) ?
                        $.fn.dataTable.ext.buttons.pdfHtml5.action.call(self, e, dt, button, config) :
                        $.fn.dataTable.ext.buttons.pdfFlash.action.call(self, e, dt, button, config);
                } else if (button[0].className.indexOf('buttons-print') >= 0) {
                    $.fn.dataTable.ext.buttons.print.action(e, dt, button, config);
                }
                dt.one('preXhr', function (e, s, data) {
                    // DataTables thinks the first item displayed is index 0, but we're not drawing that.
                    // Set the property to what it was before exporting.
                    settings._iDisplayStart = oldStart;
                    data.start = oldStart;
                });
                // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
                setTimeout(dt.ajax.reload, 0);
                // Prevent rendering of the full data to the DOM
                return false;
            });
        });
        // Requery the server with the new one-time export settings
        dt.ajax.reload();
    };

    //------------------------------------------------------------------------------------------------//
});
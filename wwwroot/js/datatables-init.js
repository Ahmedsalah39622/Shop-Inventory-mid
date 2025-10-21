$(document).ready(function () {
    // Initialize DataTables only for tables explicitly marked with the `datatable` class.
    // This avoids initializing layout or small helper tables that don't have matching
    // thead/tbody column counts which causes the "Incorrect column count" warning.
    $('.datatable').each(function() {
        var $table = $(this);
        // Use the raw DOM node when checking/is-initialized
        if (!$.fn.DataTable.isDataTable($table[0])) {
            $table.DataTable({
                "pageLength": 25,
                "order": [[1, "asc"]],
                "responsive": true,
                "language": {
                    "search": "Search:",
                    "lengthMenu": "Show _MENU_ entries",
                    "info": "Showing _START_ to _END_ of _TOTAL_ entries",
                    "paginate": {
                        "first": "First",
                        "last": "Last",
                        "next": "Next",
                        "previous": "Previous"
                    }
                }
            });
        }
    });
});
$(document).ready(function () {
    $('#returnDate').hide();
    $('#roundTripRadio').change(function () {
        if ($('#optionRoundTrip').is(':checked')) {
            $('#returnDate').show();
        }
        else { $('#returnDate').hide(); }
    });


    $('#formDate').change(function () {
        var departureDate = new Date($('#DepartureTime').val());
        var returnDate = new Date($('#ReturnDepartureTime').val());
        if (departureDate > returnDate && $('#optionRoundTrip').is(':checked')) {
            $('#dateError').show();
            $("#btnSubmit").attr("disabled", true);
        }
        else {
            var errorElement = document.getElementById("dateError");
            if (errorElement !== null) {
                $('#dateError').hide();
                $("#btnSubmit").attr("disabled", false);
            }
        }
    });
});


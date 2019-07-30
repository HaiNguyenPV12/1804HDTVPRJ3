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

    $('#btnSwap').click(function (e) {
        e.preventDefault();
        //get original locations
        var departure = $("#combobox").val();
        var destination = $("#combobox1").val();
        var departureText = $("#combobox option:selected").text();
        var destinationText = $("#combobox1 option:selected").text();
        //console.log(departure + " - " + destination);

        //reverse locations
        $("#combobox").val(destination);
        $("#combobox1").val(departure);

        //reverse location of input fields (which is different from hidden comboboxes)
        var departureInput = document.getElementsByName("autoComplete")[0];
        departureInput.value = destinationText;
        var destinationInput = document.getElementsByName("autoComplete")[1];
        destinationInput.value = departureText;
        //console.log("--> " + $("#combobox").val() + " - " + $("#combobox1").val());
    });
});


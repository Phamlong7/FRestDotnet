$(document).ready(function () {
    $(document).on('click', '.btn-add-cart', function () {
        $.blockUI(); // Block UI during AJAX call

        const id = $(this).data('dish-id'); // Get the dish ID (Ensure this matches your button's data attribute)
        console.log("Adding to cart: " + id); // Log the ID being added

        if (!id) {
            console.error("Dish ID not found!");
            $.unblockUI(); // Unblock UI if there's an error
            return;
        }

        $.ajax({
            url: '/cart/add', // Ensure this URL matches your route
            method: 'POST',
            data: { DishId: id, Quantity: 1 }, // Ensure property names match your CartModel
            success: function (count) {
                console.log("Response count: " + count); // Log the response count
                if (count === -1) {
                    showToast('Error adding item to cart', 'error'); // Show error message
                } else {
                    $('.cart-number').text(count); // Update the cart number in the navbar
                    showToast('Dish added to cart successfully!', 'success'); // Show success message
                }
                $.unblockUI(); // Unblock UI after AJAX call
            },
            error: function (xhr, status, error) {
                console.error("AJAX error: " + status + " - " + error); // Log AJAX error
                showToast('Failed to add dish to cart', 'error'); // Handle AJAX error
                $.unblockUI(); // Unblock UI if there's an error
            }
        });
    });


    function showToast(message, type) {
        // Create a div for the toast and add text
        const toast = $('<div></div>').text(message);

        // Add inline styles for positioning and appearance
        toast.css({
            "position": "fixed",
            "top": "20px", // Position it at the bottom
            "right": "20px", // Position it at the right corner
            "background-color": (type === 'success' ? '#28a745' : '#dc3545'), // Green for success, Red for error
            "color": "#fff", // White text color
            "padding": "15px",
            "border-radius": "5px",
            "z-index": 10000, // High z-index to ensure it appears on top
            "opacity": "0.9",
            "font-size": "16px",
            "box-shadow": "0 2px 10px rgba(0,0,0,0.1)"
        });

        // Append the toast to the body
        $('body').append(toast);

        // Fade out the toast after 3 seconds
        setTimeout(() => {
            toast.fadeOut(() => {
                toast.remove(); // Remove the toast after it fades out
            });
        }, 3000); // 3-second delay before fading out
    }
});

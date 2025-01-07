<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration {
    public function up(): void
    {
        Schema::create('follows', function (Blueprint $table) {
            $table->id();
            $table->integer('following_id'); // フォローする側のユーザーID
            $table->integer('followee_id'); // フォローされる側のユーザーID
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('follows');
    }
};
